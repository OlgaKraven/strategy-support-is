from flask import Flask
from .extensions import db, jwt, cors
from .middleware.error_handler import register_error_handlers
from .middleware.request_id import register_request_id
from .routes.auth import auth_bp
from .routes.events import events_bp
from .routes.health import health_bp
from .routes.leaderboard import leaderboard_bp
from .routes.profile import profile_bp
from .routes.match import match_bp


def create_app(config=None):
    app = Flask(__name__)
    app.config.from_object(config or "app.config.Config")

    db.init_app(app)
    jwt.init_app(app)
    cors.init_app(app, resources={r"/*": {"origins": "*"}})

    register_error_handlers(app)
    register_request_id(app)

    app.register_blueprint(auth_bp)
    app.register_blueprint(events_bp)
    app.register_blueprint(health_bp)
    app.register_blueprint(leaderboard_bp)
    app.register_blueprint(profile_bp)
    app.register_blueprint(match_bp)

    return app
