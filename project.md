# Project Build Prompt — Integrated Library Management & Exam Seat Allocation System

Paste this whole document into Codex (VS Code) as your starting instruction. It tells the agent what to build, in what order, and how to verify each step before moving to the next.

---

## 0. Role & Working Style

You are building a full-stack web application for a university Software Development Project (SDE) course. Work **incrementally, in the exact phase order below**. After each phase:
1. Make sure the project builds/runs with no errors.
2. Run a quick manual or automated sanity check (list what you checked).
3. `git add` + `git commit` with a clear message before starting the next phase.
4. Summarize what was built and what's left, then wait for confirmation before continuing to the next phase (unless told to run through all phases automatically).

Do not skip ahead to advanced features (real-time, GraphQL, AI) before the core CRUD/auth backbone works end-to-end.

---

## 1. Tech Stack (fixed — do not substitute without asking)

- **Backend:** C# / ASP.NET Core (.NET 8) Web API
- **Database:** PostgreSQL, accessed via `Npgsql.EntityFrameworkCore.PostgreSQL` (EF Core). Management via pgAdmin (external tool, not part of the codebase).
- **Frontend:** React (Vite), SASS for styling, GSAP for animations
- **Real-time:** SignalR (WebSockets) for comments, likes, and seat-status updates
- **API layer:** REST endpoints for CRUD; GraphQL (HotChocolate) as an additional query layer once REST is stable
- **Reporting:** QuestPDF (NOT Crystal Reports — avoid licensing/setup overhead) for seat charts and circulation reports
- **AI feature (required):** pick ONE and implement it fully rather than half-implementing several:
  - Sentiment analysis on book comments (ML.NET or a hosted model call), OR
  - Book recommendation system based on a student's borrowing history, OR
  - TensorFlow Lite prediction for book demand / exam attendance (only if the .NET bridge proves manageable — otherwise fall back to one of the above)
- **Auth:** ASP.NET Core Identity or custom JWT auth, with BCrypt password hashing, role-based access control (Admin, Librarian, Exam Coordinator)
- **Maps/Media:** Google Maps (or OpenStreetMap/Leaflet as a free alternative) embed, YouTube iframe embed

---

## 2. Shared Data Model

Implement these entities first (see ER design below) since both modules depend on the shared `Student/Member` table:

- `Student` (student_id PK, name, roll_no, dept, semester, contact, role, is_verified)
- `Book` (book_id PK, title, author, isbn, genre, copies_available)
- `IssueRecord` (issue_id PK, book_id FK, student_id FK, issue_date, due_date, return_date, fine_amount)
- `Comment` (comment_id PK, book_id FK, student_id FK, comment_text, created_at)
- `Reaction` (reaction_id PK, book_id FK, student_id FK, type: like/dislike)
- `Exam` (exam_id PK, course, semester, exam_date, time_slot)
- `Room` (room_id PK, room_no, capacity, bench_layout)
- `SeatAllocation` (seat_id PK, exam_id FK, room_id FK, student_id FK, bench_no, seat_no, invigilator_id FK)
- `Invigilator` (staff_id PK, name, dept)

---

## 3. Phase-by-Phase Build Order

### Phase 1 — Environment & Skeleton
- Scaffold `dotnet new webapi -n LibraryExamAPI`.
- Add `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.Design`, and `dotnet-ef` tooling.
- Create the `AppDbContext` with all entities above, add the connection string to `appsettings.Development.json` (use a placeholder password, note that it needs to be filled locally), run the first migration, and confirm tables appear in pgAdmin.
- Scaffold the React app with Vite (`npm create vite@latest client -- --template react`), install sass and gsap, confirm it runs and can hit a test `/api/ping` endpoint on the backend (set up CORS for the dev server port).
- Initialize git, commit.

### Phase 2 — Auth & RBAC
- Implement registration, login, logout, and password reset endpoints.
- Add BCrypt password hashing.
- Add role-based access control (Admin, Librarian, Exam Coordinator) with JWT or cookie-based auth — protect endpoints accordingly.
- Add real-time username/email availability check endpoint (called as the user types on the frontend).
- Add a password-strength indicator component on the frontend.
- Add client-side and server-side form validation on the auth forms.
- Build the frontend Login/Register/Reset pages.

### Phase 3 — Library Management Module (CRUD core)
- Book CRUD endpoints + admin UI (add/edit/delete/categorize).
- Member registry tied to `Student`.
- Issue/return workflow with borrowing-limit enforcement and live stock updates.
- Fine calculation engine (configurable per-day rate).
- Search/filter/sort by title, author, ISBN, availability.
- Pagination (or infinite scroll) on the book list.
- File/image upload with preview for book covers.
- Reports: circulation report, most-borrowed titles, overdue list.

### Phase 4 — Exam Seat Allocation Module (CRUD core)
- Room & bench setup CRUD.
- Candidate import/selection by course/semester.
- Seat allocation algorithm: sort by roll/section, interleave across benches/rooms so same-section students aren't adjacent, round-robin across rooms.
- Invigilator assignment.
- Manual override of individual seat assignments after auto-allocation.
- Printable room-wise and roll-wise seat charts + attendance sheets via QuestPDF.

### Phase 5 — Real-Time Layer (SignalR)
- SignalR hub for live comments on books.
- SignalR hub for live like/dislike counters.
- SignalR hub/broadcast for live seat-plan fill status on the admin dashboard.
- Live due-date / seat-confirmation notifications.
- GSAP animations for these live UI updates (subtle transitions, not distracting).

### Phase 6 — Dashboard, GraphQL, AI Feature
- Interactive admin dashboard with charts/statistics (books issued, overdue fines, upcoming exam jobs) — updates live via SignalR where relevant.
- GraphQL endpoint (HotChocolate) exposing the same core data as the REST API, for at least the book/student/seat-allocation queries.
- Implement the chosen AI feature end-to-end (see Section 1) and surface its output somewhere visible in the UI (e.g., "Recommended for you" on the book list, or a sentiment badge on comments).

### Phase 7 — Polish & Supporting Features
- Date/time picker for exam scheduling.
- Responsive design pass (mobile/tablet/desktop) across all pages.
- Dark mode / theme switch (optional, do if time allows).
- Toast notifications, modal dialogs, loading states across the app.
- Google Maps/OpenStreetMap embed for library branches/exam centers.
- YouTube embed for orientation/tutorial content.
- Email verification / OTP flow on registration.
- Audit trail logging for issue/return and seat-plan generation actions.
- Basic hardening: parameterized queries (EF Core handles this by default — verify no raw SQL string concatenation), output encoding against XSS, CSRF tokens on state-changing forms.

### Phase 8 — Testing & Wrap-up
- Unit tests (xUnit/MSTest) for fine calculation and seat allocation logic.
- Integration tests for key API endpoints against a test Postgres database.
- Manual test pass on edge cases: odd candidate counts, room over-capacity, duplicate registration attempts, expired sessions.
- Write/update the README with setup instructions (.NET SDK version, Postgres setup, `dotnet ef database update`, `npm install && npm run dev`).
- (Optional) Deploy backend + frontend to a free host (Render/Railway/Azure free tier + Vercel/Netlify for the frontend).

---

## 4. Instructions to the Agent

- Confirm before each phase whether to proceed automatically or pause for review.
- If a required NuGet/npm package fails to install or a feature (e.g., TensorFlow Lite bridge) turns out to be impractical in the time available, say so explicitly and propose the documented fallback rather than silently skipping it.
- Keep commits scoped to one phase (or one clear sub-feature) at a time so the project history reads as a build log.
- Flag anything from Section 1's feature list that ends up **not implemented** at the end, so it's clear what's missing before submission.
