from sqlalchemy import text
from app.middleware.error_handler import AppError

def register_event_or_conflict(session, user_id: int, event_id: str, event_type: str):
    # processed_events имеет UNIQUE(user_id, event_id)
    sql = text("""
        INSERT INTO processed_events (user_id, event_id, event_type, processed_at)
        VALUES (:user_id, :event_id, :event_type, NOW(3))
    """)
    try:
        session.execute(sql, {"user_id": user_id, "event_id": event_id, "event_type": event_type})
    except Exception:
        # В MySQL можно ловить IntegrityError, но в учебном MVP достаточно AppError
        raise AppError(code="EVENT_REJECTED", message="Duplicate eventId", http_status=409)