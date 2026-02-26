from flask import g, request, jsonify
from app.services.events_service import accept_event_service

def post_event_controller():
    user_id = g.user["userId"]
    payload = request.get_json()
    result = accept_event_service(user_id, payload)
    return jsonify({"ok": True, "data": result}), 200