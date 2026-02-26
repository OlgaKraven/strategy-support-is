from app.extensions import db
from app.repositories.dedupe_repo import register_event_or_conflict
from app.repositories.events_repo import insert_game_event


def accept_event_service(user_id: int, payload: dict) -> dict:
    event_id = payload["eventId"]
    event_type = payload["eventType"]
    session_id = payload.get("sessionId")
    event_payload = payload.get("payload") or {}

    try:
        register_event_or_conflict(db.session, user_id=user_id, event_id=event_id, event_type=event_type)
        insert_game_event(db.session, user_id=user_id, session_id=session_id, event_type=event_type, payload=event_payload)
        db.session.commit()
    except Exception:
        db.session.rollback()
        raise

    return {"eventId": event_id, "status": "accepted"}
