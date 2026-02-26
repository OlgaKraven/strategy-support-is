from flask_jwt_extended import create_access_token
from app.extensions import db
from app.middleware.error_handler import AppError
from app.repositories.users_repo import find_user_by_email, create_user, get_user_roles
from app.repositories.roles_repo import assign_role_to_user
from app.repositories.progress_repo import ensure_progress_row
import bcrypt


def register_service(payload: dict) -> dict:
    email = payload["email"]
    password = payload["password"]
    nickname = payload["nickname"]

    if find_user_by_email(email) is not None:
        raise AppError(code="CONFLICT", message="Email already registered", http_status=409)

    hashed = bcrypt.hashpw(password.encode(), bcrypt.gensalt()).decode()

    try:
        user_id = create_user(db.session, email=email, password=hashed, nickname=nickname)
        assign_role_to_user(db.session, user_id=user_id, role_name="player")
        ensure_progress_row(db.session, user_id=user_id)
        db.session.commit()
    except Exception:
        db.session.rollback()
        raise

    return {"userId": user_id, "email": email, "nickname": nickname}


def login_service(payload: dict) -> dict:
    email = payload["email"]
    password = payload["password"]

    user = find_user_by_email(email)
    if user is None:
        raise AppError(code="UNAUTHORIZED", message="Invalid credentials", http_status=401)

    if not bcrypt.checkpw(password.encode(), user["password"].encode()):
        raise AppError(code="UNAUTHORIZED", message="Invalid credentials", http_status=401)

    if user["is_banned"]:
        raise AppError(code="FORBIDDEN", message="Account is banned", http_status=403)

    roles = get_user_roles(user["id"])
    access_token = create_access_token(
        identity=user["id"],
        additional_claims={"roles": roles}
    )

    return {"accessToken": access_token, "userId": user["id"], "nickname": user["nickname"]}
