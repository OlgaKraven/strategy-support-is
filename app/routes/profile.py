from flask import Blueprint
from app.controllers.profile_controller import profile_controller
from app.middleware.auth_jwt import auth_required

profile_bp = Blueprint("profile", __name__, url_prefix="/api/v1")

@profile_bp.get("/profile")
@auth_required()
def get_profile():
    return profile_controller()
