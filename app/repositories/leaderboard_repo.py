from sqlalchemy import text
from app.extensions import db

def upsert_leaderboard_score(session, user_id: int, board_code: str, season: int, score: int):
    sql = text("""
        INSERT INTO leaderboard_scores (user_id, board_code, season, score)
        VALUES (:user_id, :board_code, :season, :score)
        ON DUPLICATE KEY UPDATE
          score = GREATEST(score, VALUES(score)),
          updated_at = CURRENT_TIMESTAMP(3);
    """)
    session.execute(sql, {"user_id": user_id, "board_code": board_code, "season": season, "score": score})

def get_leaderboard(board_code: str, season: int, limit: int):
    sql = text("""
        SELECT ls.user_id AS userId, u.nickname AS nickname, ls.score AS score
        FROM leaderboard_scores ls
        JOIN users u ON u.id = ls.user_id
        WHERE ls.board_code = :board_code AND ls.season = :season
        ORDER BY ls.score DESC
        LIMIT :limit
    """)
    rows = db.session.execute(sql, {"board_code": board_code, "season": season, "limit": limit}).mappings().all()

    items = []
    rank = 1
    for r in rows:
        items.append({"rank": rank, "userId": r["userId"], "nickname": r["nickname"], "score": r["score"]})
        rank += 1
    return items