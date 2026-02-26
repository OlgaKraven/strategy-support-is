from flask import Blueprint
from app.controllers.leaderboard_controller import leaderboard_controller
from app.middleware.validate_api import validate_leaderboard_query

leaderboard_bp = Blueprint("leaderboard", __name__, url_prefix="/api/v1")

@leaderboard_bp.get("/leaderboard")
@validate_leaderboard_query
def get_leaderboard():
    return leaderboard_controller()
