from sqlalchemy import text
from app.middleware.error_handler import AppError

def ensure_match_ownership_started(session, match_id: int, user_id: int):
    sql = text("""
        SELECT id, user_id, status
        FROM matches
        WHERE id = :match_id
    """)
    row = session.execute(sql, {"match_id": match_id}).mappings().first()
    if row is None:
        raise AppError(code="NOT_FOUND", message="Match not found", http_status=404)
    if row["user_id"] != user_id:
        raise AppError(code="FORBIDDEN", message="Match does not belong to user", http_status=403)
    if row["status"] != "started":
        raise AppError(code="CONFLICT", message="Match is not in started state", http_status=409)

def finish_match_row(session, match_id: int, is_win: bool, score: int, duration_seconds: int):
    sql1 = text("""
        UPDATE matches
        SET status = 'finished',
            ended_at = NOW(3)
        WHERE id = :match_id
    """)
    session.execute(sql1, {"match_id": match_id})

    sql2 = text("""
        INSERT INTO battle_results (match_id, is_win, score, duration_seconds)
        VALUES (:match_id, :is_win, :score, :duration)
        ON DUPLICATE KEY UPDATE
          is_win = VALUES(is_win),
          score = VALUES(score),
          duration_seconds = VALUES(duration_seconds)
    """)
    session.execute(sql2, {
        "match_id": match_id,
        "is_win": 1 if is_win else 0,
        "score": score,
        "duration": duration_seconds
    })