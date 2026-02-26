from flask import g, request, jsonify
from app.services.match_service import finish_match_service

def finish_match_controller():
    user_id = g.user["userId"]
    payload = request.get_json()
    result = finish_match_service(user_id=user_id, payload=payload)
    return jsonify({"ok": True, "data": result}), 200