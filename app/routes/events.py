from flask import Blueprint
from app.controllers.events_controller import post_event_controller
from app.middleware.auth_jwt import auth_required
from app.middleware.validate_api import validate_event

events_bp = Blueprint("events", __name__, url_prefix="/api/v1")

@events_bp.post("/events")
@auth_required(roles=["player"])
@validate_event
def post_event():
    return post_event_controller()