from functools import wraps
from flask import request
from .error_handler import AppError

def _bad_body(details):
    raise AppError(code="VALIDATION_ERROR", message="Invalid request body", http_status=400, details=details)

def _bad_query(details):
    raise AppError(code="VALIDATION_ERROR", message="Invalid query params", http_status=400, details=details)

def validate_register(fn):
    @wraps(fn)
    def wrapper(*args, **kwargs):
        data = request.get_json(silent=True)
        if not isinstance(data, dict):
            _bad_body({"body": "JSON object expected"})

        email = data.get("email")
        password = data.get("password")
        nickname = data.get("nickname")

        if not isinstance(email, str) or "@" not in email:
            _bad_body({"email": "valid email required"})
        if not isinstance(password, str) or len(password) < 6:
            _bad_body({"password": "string length >= 6 required"})
        if not isinstance(nickname, str) or len(nickname) < 2:
            _bad_body({"nickname": "string length >= 2 required"})

        return fn(*args, **kwargs)

    return wrapper

def validate_login(fn):
    @wraps(fn)
    def wrapper(*args, **kwargs):
        data = request.get_json(silent=True)
        if not isinstance(data, dict):
            _bad_body({"body": "JSON object expected"})

        email = data.get("email")
        password = data.get("password")

        if not isinstance(email, str) or "@" not in email:
            _bad_body({"email": "valid email required"})
        if not isinstance(password, str) or len(password) < 1:
            _bad_body({"password": "string required"})

        return fn(*args, **kwargs)

    return wrapper

def validate_event(fn):
    @wraps(fn)
    def wrapper(*args, **kwargs):
        data = request.get_json(silent=True)
        if not isinstance(data, dict):
            _bad_body({"body": "JSON object expected"})

        event_id = data.get("eventId")
        event_type = data.get("eventType")
        session_id = data.get("sessionId")
        client_time = data.get("clientTime")
        payload = data.get("payload")

        if not isinstance(event_id, str) or len(event_id) < 8:
            _bad_body({"eventId": "uuid string required"})
        if not isinstance(event_type, str) or len(event_type) < 1:
            _bad_body({"eventType": "string required"})
        if session_id is not None and not isinstance(session_id, int):
            _bad_body({"sessionId": "int or null required"})
        if client_time is not None and not isinstance(client_time, str):
            _bad_body({"clientTime": "string or null required"})
        if payload is not None and not isinstance(payload, dict):
            _bad_body({"payload": "object or null required"})

        return fn(*args, **kwargs)

    return wrapper

def validate_leaderboard_query(fn):
    @wraps(fn)
    def wrapper(*args, **kwargs):
        board_code = request.args.get("boardCode")
        season = request.args.get("season")
        limit = request.args.get("limit")

        if not isinstance(board_code, str) or len(board_code) < 1:
            _bad_query({"boardCode": "string required"})
        if season is None or not season.isdigit():
            _bad_query({"season": "int required"})
        if limit is None or not limit.isdigit():
            _bad_query({"limit": "int required"})

        lim = int(limit)
        if lim < 1 or lim > 1000:
            _bad_query({"limit": "1..1000 required"})

        return fn(*args, **kwargs)

    return wrapper

def validate_match_finish(fn):
    @wraps(fn)
    def wrapper(*args, **kwargs):
        data = request.get_json(silent=True)
        if not isinstance(data, dict):
            _bad_body({"body": "JSON object expected"})

        match_id = data.get("matchId")
        result = data.get("result")

        if not isinstance(match_id, int):
            _bad_body({"matchId": "int required"})
        if not isinstance(result, dict):
            _bad_body({"result": "object required"})

        is_win = result.get("isWin")
        score = result.get("score")
        duration = result.get("durationSeconds")
        power = result.get("powerDelta")

        if not isinstance(is_win, bool):
            _bad_body({"result.isWin": "bool required"})
        if not isinstance(score, int) or score < 0:
            _bad_body({"result.score": "int >= 0 required"})
        if not isinstance(duration, int) or duration < 0:
            _bad_body({"result.durationSeconds": "int >= 0 required"})
        if power is not None and (not isinstance(power, int)):
            _bad_body({"result.powerDelta": "int or null required"})

        return fn(*args, **kwargs)

    return wrapper