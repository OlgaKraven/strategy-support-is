from sqlalchemy import text

def assign_role_to_user(session, user_id: int, role_name: str):
    role_sql = text("SELECT id FROM roles WHERE name = :name")
    role = session.execute(role_sql, {"name": role_name}).mappings().first()

    if role is None:
        ins_role = text("INSERT INTO roles (name) VALUES (:name)")
        res = session.execute(ins_role, {"name": role_name})
        role_id = res.lastrowid
    else:
        role_id = role["id"]

    link_sql = text("""
        INSERT INTO user_roles (user_id, role_id, assigned_at)
        VALUES (:user_id, :role_id, NOW(3))
        ON DUPLICATE KEY UPDATE assigned_at = assigned_at
    """)
    session.execute(link_sql, {"user_id": user_id, "role_id": role_id})