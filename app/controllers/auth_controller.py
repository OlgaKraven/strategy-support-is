from flask import request, jsonify
from app.services.auth_service import register_service, login_service

def register_controller():
    payload = request.get_json()
    result = register_service(payload)
    return jsonify({"ok": True, "data": result}), 201

def login_controller():
    payload = request.get_json()
    result = login_service(payload)
    return jsonify({"ok": True, "data": result}), 200