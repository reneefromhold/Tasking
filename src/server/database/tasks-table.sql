-- Tasks definition (v1)

CREATE TABLE Tasks (
    id TEXT NOT NULL,
    categoryId TEXT,
    title TEXT NOT NULL,
    description TEXT,
    creator TEXT,
    assignee TEXT,
    createDate TEXT NOT NULL,
    dueDate TEXT,
    CONSTRAINT Tasks_PK PRIMARY KEY (id),
    CONSTRAINT FK_Tasks_User_Creator FOREIGN KEY (creator) REFERENCES "User"(id),
    CONSTRAINT FK_Tasks_User_Assignee FOREIGN KEY (assignee) REFERENCES "User"(id),
    CONSTRAINT FK_Tasks_Categories_CategoryId FOREIGN KEY (categoryId) REFERENCES Categories(id)
);

CREATE INDEX idx_tasks_assignee
ON Tasks(assignee);
