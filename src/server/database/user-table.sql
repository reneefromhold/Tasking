-- User definition (v1)

CREATE TABLE "User" (
    id TEXT PRIMARY KEY NOT NULL,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE
);

CREATE INDEX idx_user_email
ON "User"(email);
