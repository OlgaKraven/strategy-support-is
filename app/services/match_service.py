from app.extensions import db
from app.repositories.matches_repo import ensure_match_ownership_started, finish_match_row
from app.repositories.progress_repo import ensure_progress_row, lock_progress_row, add_progress_rewards
from app.repositories.stats_repo import upsert_daily_stats
from app.repositories.leaderboard_repo import upsert_leaderboard_score

_XP_WIN = 100
_XP_LOSS = 30
_SOFT_WIN = 50
_SOFT_LOSS = 10
_BOARD_CODE = "default"
_SEASON = 1


def finish_match_service(user_id: int, payload: dict) -> dict:
    match_id = payload["matchId"]
    result = payload["result"]
    is_win = result["isWin"]
    score = result["score"]
    duration = result["durationSeconds"]

    xp_gained = _XP_WIN if is_win else _XP_LOSS
    soft_gained = _SOFT_WIN if is_win else _SOFT_LOSS

    try:
        ensure_match_ownership_started(db.session, match_id=match_id, user_id=user_id)
        finish_match_row(db.session, match_id=match_id, is_win=is_win, score=score, duration_seconds=duration)
        ensure_progress_row(db.session, user_id=user_id)
        lock_progress_row(db.session, user_id=user_id)
        add_progress_rewards(db.session, user_id=user_id, xp=xp_gained, soft_currency=soft_gained)
        upsert_daily_stats(db.session, user_id=user_id, playtime_seconds=duration, is_win=is_win, score=score)
        upsert_leaderboard_score(db.session, user_id=user_id, board_code=_BOARD_CODE, season=_SEASON, score=score)
        db.session.commit()
    except Exception:
        db.session.rollback()
        raise

    return {
        "matchId": match_id,
        "isWin": is_win,
        "xpGained": xp_gained,
        "softCurrencyGained": soft_gained,
    }
