from flask import g, jsonify
from app.services.profile_service import profile_service

def profile_controller():
    user_id = g.user["userId"]
    result = profile_service(user_id)
    return jsonify({"ok": True, "data": result}), 200