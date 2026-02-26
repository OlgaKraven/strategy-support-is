from sqlalchemy import text
from app.middleware.error_handler import AppError
from app.extensions import db

def ensure_progress_row(session, user_id: int):
    sql = text("INSERT IGNORE INTO player_progress (user_id) VALUES (:user_id)")
    session.execute(sql, {"user_id": user_id})

def get_progress_by_user(user_id: int) -> dict:
    sql = text("""
        SELECT level, xp, soft_currency, hard_currency
        FROM player_progress
        WHERE user_id = :user_id
    """)
    row = db.session.execute(sql, {"user_id": user_id}).mappings().first()
    if row is None:
        return {"level": 1, "xp": 0, "softCurrency": 0, "hardCurrency": 0}
    return {
        "level": row["level"],
        "xp": row["xp"],
        "softCurrency": row["soft_currency"],
        "hardCurrency": row["hard_currency"],
    }

def lock_progress_row(session, user_id: int):
    sql = text("""
        SELECT user_id, xp, soft_currency
        FROM player_progress
        WHERE user_id = :user_id
        FOR UPDATE
    """)
    row = session.execute(sql, {"user_id": user_id}).fetchone()
    if row is None:
        raise AppError(code="PROGRESS_NOT_FOUND", message="player_progress row not found", http_status=404)
    return row

def add_progress_rewards(session, user_id: int, xp: int, soft_currency: int):
    sql = text("""
        UPDATE player_progress
        SET xp = xp + :xp,
            soft_currency = soft_currency + :soft
        WHERE user_id = :user_id
    """)
    session.execute(sql, {"xp": xp, "soft": soft_currency, "user_id": user_id})