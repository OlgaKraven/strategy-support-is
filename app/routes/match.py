from flask import Blueprint
from app.controllers.match_controller import finish_match_controller
from app.middleware.auth_jwt import auth_required
from app.middleware.validate_api import validate_match_finish

match_bp = Blueprint("match", __name__, url_prefix="/api/v1")

@match_bp.post("/match/finish")
@auth_required(roles=["player"])
@validate_match_finish
def finish_match():
    return finish_match_controller()
