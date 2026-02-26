from sqlalchemy import text
from app.extensions import db

def find_user_by_email(email: str):
    sql = text("SELECT id, email, password, nickname, is_banned FROM users WHERE email = :email")
    row = db.session.execute(sql, {"email": email}).mappings().first()
    if row is None:
        return None
    return {
        "id": row["id"],
        "email": row["email"],
        "password": row["password"],
        "nickname": row["nickname"],
        "is_banned": bool(row["is_banned"]),
    }

def create_user(session, email: str, password: str, nickname: str) -> int:
    sql = text("""
        INSERT INTO users (email, password, nickname, created_at, is_banned)
        VALUES (:email, :password, :nickname, NOW(3), 0)
    """)
    res = session.execute(sql, {"email": email, "password": password, "nickname": nickname})
    return res.lastrowid

def get_user_by_id(user_id: int):
    sql = text("SELECT id, email, nickname, is_banned FROM users WHERE id = :id")
    row = db.session.execute(sql, {"id": user_id}).mappings().first()
    if row is None:
        return None
    return {"id": row["id"], "email": row["email"], "nickname": row["nickname"], "is_banned": bool(row["is_banned"])}

def get_user_roles(user_id: int):
    sql = text("""
        SELECT r.name AS role_name
        FROM user_roles ur
        JOIN roles r ON r.id = ur.role_id
        WHERE ur.user_id = :user_id
    """)
    rows = db.session.execute(sql, {"user_id": user_id}).mappings().all()
    return [r["role_name"] for r in rows] or ["player"]