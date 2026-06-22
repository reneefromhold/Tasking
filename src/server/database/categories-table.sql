-- Categories definition (v1)

CREATE TABLE Categories (
    id TEXT NOT NULL,
    category TEXT NOT NULL,
    CONSTRAINT Categories_PK PRIMARY KEY (id),
    CONSTRAINT UQ_Categories_Category UNIQUE (category)
);
