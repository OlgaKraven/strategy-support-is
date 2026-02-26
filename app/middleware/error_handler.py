from dataclasses import dataclass
from typing import Any, Optional
from flask import jsonify, g
from werkzeug.exceptions import HTTPException


@dataclass
class AppError(Exception):
    code: str
    message: str
    http_status: int = 400
    details: Optional[Any] = None


def _error_response(code: str, message: str, request_id: str, details=None):
    body = {
        "ok": False,
        "error": {
            "code": code,
            "message": message,
            "requestId": request_id
        }
    }
    if details is not None:
        body["error"]["details"] = details
    return body


def register_error_handlers(app):
    @app.errorhandler(AppError)
    def handle_app_error(err: AppError):
        rid = getattr(g, "request_id", "-")
        app.logger.warning("AppError %s rid=%s msg=%s", err.code, rid, err.message)
        return jsonify(_error_response(err.code, err.message, rid, err.details)), err.http_status

    # ✅ ВАЖНО: корректно отдаём HTTPException (404/405/...)
    @app.errorhandler(HTTPException)
    def handle_http_exception(err: HTTPException):
        rid = getattr(g, "request_id", "-")
        app.logger.info("HTTPException %s rid=%s path=%s", err.code, rid, getattr(err, "name", ""))
        code = f"HTTP_{err.code}"
        message = err.description  # короткое описание
        return jsonify(_error_response(code, message, rid)), err.code

    # ✅ Всё остальное — 500
    @app.errorhandler(Exception)
    def handle_exception(err: Exception):
        rid = getattr(g, "request_id", "-")
        app.logger.exception("Unhandled error rid=%s", rid)
        return jsonify(_error_response("INTERNAL_ERROR", "Unexpected error", rid)), 500