export default function StudentActivity({ recommendedBooks, libraryIssues, examAllocations, active }) {
  if (!active) return null;
  return <>
    {recommendedBooks.length > 0 && <section className="section active"><div className="section-header" style={{ marginTop: '2rem' }}><h1>Recommended for you</h1></div><div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '1.5rem' }}>{recommendedBooks.map((book) => <div className="card" key={book.bookId}><div className="card-header"><h3>{book.title}</h3></div><div className="card-body"><p><strong>{book.author}</strong></p><p>Genre: {book.genre}</p><p>Copies available: {book.copiesAvailable}</p></div></div>)}</div></section>}
    {examAllocations.length > 0 && <section className="section active"><div className="section-header" style={{ marginTop: '2rem' }}><h1>My exam seat plan</h1></div><div className="table-container"><table><thead><tr><th>Course</th><th>Date</th><th>Time</th><th>Room</th><th>Bench</th><th>Seat</th></tr></thead><tbody>{examAllocations.map((allocation) => <tr key={allocation.seatId}><td>{allocation.course}</td><td>{new Date(allocation.examDate).toLocaleDateString()}</td><td>{allocation.timeSlot}</td><td>{allocation.roomNo}</td><td>{allocation.benchNo}</td><td>{allocation.seatNo}</td></tr>)}</tbody></table></div></section>}
  </>;
}
