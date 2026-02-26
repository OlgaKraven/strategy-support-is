from sqlalchemy import text

def upsert_daily_stats(session, user_id: int, playtime_seconds: int, is_win: bool, score: int):
    wins = 1 if is_win else 0
    losses = 0 if is_win else 1

    sql = text("""
        INSERT INTO statistics_daily (user_id, day, sessions_count, events_count, playtime_seconds, wins, losses, score_sum)
        VALUES (:user_id, CURDATE(), 0, 1, :playtime, :wins, :losses, :score)
        ON DUPLICATE KEY UPDATE
          events_count = events_count + 1,
          playtime_seconds = playtime_seconds + VALUES(playtime_seconds),
          wins = wins + VALUES(wins),
          losses = losses + VALUES(losses),
          score_sum = score_sum + VALUES(score_sum);
    """)
    session.execute(sql, {
        "user_id": user_id,
        "playtime": playtime_seconds,
        "wins": wins,
        "losses": losses,
        "score": score
    })