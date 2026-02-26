from flask import request, jsonify
from app.services.leaderboard_service import leaderboard_service

def leaderboard_controller():
    board_code = request.args.get("boardCode")
    season = int(request.args.get("season", 0))
    limit = int(request.args.get("limit", 50))

    result = leaderboard_service(board_code, season, limit)
    return jsonify({"ok": True, "data": result}), 200