export default function StudentActivity({ recommendedBooks, libraryIssues, examAllocations, examRoutine, availableBooks, active }) {
  if (!active) return null;

  const activeIssues = libraryIssues.filter((issue) => !issue.returnDate);

  return <>
    <section className="section active">
      <div className="section-header" style={{ marginTop: '2rem' }}><h1>Student home</h1><p>Overview of your books, borrowing, and exam routine</p></div>
      <div className="stats-grid">
        <div className="stat-card"><div className="stat-header"><span>Available books</span><span className="trend-badge">Live</span></div><strong>{availableBooks.length}</strong></div>
        <div className="stat-card"><div className="stat-header"><span>Borrowed books</span><span className="trend-badge">Active</span></div><strong>{libraryIssues.length}</strong></div>
        <div className="stat-card"><div className="stat-header"><span>Open issues</span><span className="trend-badge">Now</span></div><strong>{activeIssues.length}</strong></div>
        <div className="stat-card"><div className="stat-header"><span>Exam routine</span><span className="trend-badge">Total</span></div><strong>{examRoutine.length || examAllocations.length}</strong></div>
      </div>
    </section>

    <section className="section active">
      <div className="section-header" style={{ marginTop: '2rem' }}><h1>Available books</h1></div>
      {availableBooks.length > 0 ? (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: '1.5rem' }}>
          {availableBooks.map((book) => (
            <div className="card" key={book.bookId}>
              <div className="card-header"><h3>{book.title}</h3></div>
              <div className="card-body">
                <p><strong>{book.author}</strong></p>
                <p>Genre: {book.genre}</p>
                <p>Copies available: {book.copiesAvailable}</p>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="card"><div className="card-body"><p>No available books right now.</p></div></div>
      )}
    </section>

    <section className="section active">
      <div className="section-header" style={{ marginTop: '2rem' }}><h1>My borrowed books</h1></div>
      {libraryIssues.length > 0 ? (
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Book</th>
                <th>Issue date</th>
                <th>Due date</th>
                <th>Fine</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {libraryIssues.map((issue) => (
                <tr key={issue.issueId || `${issue.bookId}-${issue.issueDate}`}>
                  <td>{issue.bookTitle}</td>
                  <td>{new Date(issue.issueDate).toLocaleDateString()}</td>
                  <td>{new Date(issue.dueDate).toLocaleDateString()}</td>
                  <td>${Number(issue.fineAmount || 0)}</td>
                  <td>{issue.returnDate ? 'Returned' : issue.isOverdue ? 'Overdue' : 'Active'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="card"><div className="card-body"><p>You have not borrowed any books yet.</p></div></div>
      )}
    </section>

    {recommendedBooks.length > 0 && <section className="section active"><div className="section-header" style={{ marginTop: '2rem' }}><h1>Recommended for you</h1></div><div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1.5rem' }}>{recommendedBooks.map((book) => <div className="card" key={book.bookId}><div className="card-header"><h3>{book.title}</h3></div><div className="card-body"><p><strong>{book.author}</strong></p><p>Genre: {book.genre}</p><p>Copies available: {book.copiesAvailable}</p></div></div>)}</div></section>}

    <section className="section active">
      <div className="section-header" style={{ marginTop: '2rem' }}><h1>Exam routine</h1></div>
      {(examRoutine.length > 0 ? examRoutine : examAllocations).length > 0 ? (
        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Course</th>
                <th>Date</th>
                <th>Time</th>
                <th>Room</th>
                <th>Bench</th>
                <th>Seat</th>
              </tr>
            </thead>
            <tbody>
              {(examRoutine.length > 0 ? examRoutine : examAllocations).map((entry, index) => (
                <tr key={entry.examId || entry.seatId || `${entry.course}-${index}`}>
                  <td>{entry.course}</td>
                  <td>{new Date(entry.examDate || entry.date || Date.now()).toLocaleDateString()}</td>
                  <td>{entry.timeSlot}</td>
                  <td>{entry.roomNo || 'TBA'}</td>
                  <td>{entry.benchNo || 'TBA'}</td>
                  <td>{entry.seatNo || 'TBA'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="card"><div className="card-body"><p>No exam routine assigned yet.</p></div></div>
      )}
    </section>
  </>;
}
