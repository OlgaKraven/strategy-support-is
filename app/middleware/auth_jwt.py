from functools import wraps
from flask import g
from flask_jwt_extended import verify_jwt_in_request, get_jwt, get_jwt_identity
from .error_handler import AppError

def auth_required(roles=None):
    roles = roles or []

    def decorator(fn):
        @wraps(fn)
        def wrapper(*args, **kwargs):
            try:
                verify_jwt_in_request()
            except Exception:
                raise AppError(code="UNAUTHORIZED", message="JWT is missing/invalid/expired", http_status=401)

            user_id = get_jwt_identity()
            claims = get_jwt() or {}
            user_roles = claims.get("roles", [])

            if roles:
                ok = any(r in user_roles for r in roles)
                if not ok:
                    raise AppError(code="FORBIDDEN", message="Insufficient role", http_status=403)

            g.user = {"userId": user_id, "roles": user_roles}
            return fn(*args, **kwargs)

        return wrapper

    return decorator