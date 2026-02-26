from flask import Blueprint
from app.controllers.auth_controller import register_controller, login_controller
from app.middleware.validate_api import validate_register, validate_login

auth_bp = Blueprint("auth", __name__, url_prefix="/api/v1/auth")

@auth_bp.post("/register")
@validate_register
def register():
    return register_controller()

@auth_bp.post("/login")
@validate_login
def login():
    return login_controller()