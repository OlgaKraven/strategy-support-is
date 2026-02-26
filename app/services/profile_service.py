from app.middleware.error_handler import AppError
from app.repositories.users_repo import get_user_by_id, get_user_roles
from app.repositories.progress_repo import get_progress_by_user


def profile_service(user_id: int) -> dict:
    user = get_user_by_id(user_id)
    if user is None:
        raise AppError(code="NOT_FOUND", message="User not found", http_status=404)

    roles = get_user_roles(user_id)
    progress = get_progress_by_user(user_id)

    return {
        "userId": user["id"],
        "email": user["email"],
        "nickname": user["nickname"],
        "roles": roles,
        "progress": progress,
    }
