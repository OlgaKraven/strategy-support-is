import time
import uuid
from flask import g, request

def _gen_request_id() -> str:
    return "req_" + uuid.uuid4().hex[:8]

def register_request_id(app):
    @app.before_request
    def _before():
        g.request_id = request.headers.get("X-Request-Id") or _gen_request_id()
        g._start_ms = int(time.time() * 1000)

    @app.after_request
    def _after(response):
        duration_ms = int(time.time() * 1000) - getattr(g, "_start_ms", int(time.time() * 1000))
        response.headers["X-Request-Id"] = getattr(g, "request_id", "-")

        user_id = "-"
        if getattr(g, "user", None) and isinstance(g.user, dict):
            user_id = g.user.get("userId", "-")

        app.logger.info(
            "%s user=%s %s %s %s %sms",
            getattr(g, "request_id", "-"),
            user_id,
            request.method,
            request.path,
            response.status_code,
            duration_ms,
        )
        return response