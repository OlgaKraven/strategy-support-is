from app.repositories.leaderboard_repo import get_leaderboard


def leaderboard_service(board_code: str, season: int, limit: int) -> dict:
    items = get_leaderboard(board_code, season, limit)
    return {"boardCode": board_code, "season": season, "items": items}
