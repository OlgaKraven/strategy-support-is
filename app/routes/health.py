from flask import Blueprint, jsonify

health_bp = Blueprint("health", __name__)

@health_bp.get("/")
def root():
    return jsonify({"ok": True, "data": {"service": "strategy-support-is", "status": "running"}}), 200

@health_bp.get("/health")
def health():
    return jsonify({"ok": True, "data": {"status": "ok"}}), 200