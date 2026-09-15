import LoadingSkeleton from '../feedback/LoadingSkeleton';

export default function OverviewSection({ active, currentUser, status, loading, statCards, myLibraryIssues, myExamAllocations }) {
  return (
    <section className={`section ${active ? 'active' : ''}`}>
      <div className="section-header"><h1>Dashboard Overview</h1><p>Welcome back, {currentUser.name}</p></div>
      {status.message && <div className={`message ${status.type}`}>{status.message}</div>}
      {loading ? <LoadingSkeleton variant="stats" /> : statCards.length > 0 && <>
        <div className="stats-grid">{statCards.map((card) => <div className="stat-card" key={card.label}><div className="stat-header"><span>{card.label}</span><span className="trend-badge">{card.trend}</span></div><strong>{card.value}</strong></div>)}</div>
        {currentUser.role === 'Student' && <div className="stats-grid">
          <div className="stat-card"><div className="stat-header"><span>Books borrowed</span><span className="trend-badge">+3.2%</span></div><strong>{myLibraryIssues.length}</strong></div>
          <div className="stat-card"><div className="stat-header"><span>Open issues</span><span className="trend-badge">-1.1%</span></div><strong>{myLibraryIssues.filter((issue) => !issue.returnDate).length}</strong></div>
          <div className="stat-card"><div className="stat-header"><span>Exam allocations</span><span className="trend-badge">+5.4%</span></div><strong>{myExamAllocations.length}</strong></div>
          <div className="stat-card"><div className="stat-header"><span>Outstanding fine</span><span className="trend-badge">+0.8%</span></div><strong>${myLibraryIssues.reduce((sum, issue) => sum + (issue.fineAmount || 0), 0)}</strong></div>
        </div>}
      </>}
      <div className="profile-card">
        <div><label>Name</label><strong>{currentUser.name}</strong></div>
        <div><label>Role</label><strong>{currentUser.role}</strong></div>
        <div><label>Email</label><strong>{currentUser.email || currentUser.contact || 'N/A'}</strong></div>
        <div><label>Department</label><strong>{currentUser.dept || 'N/A'}</strong></div>
        <div><label>Semester</label><strong>{currentUser.semester ?? 'N/A'}</strong></div>
        <div><label>Roll number</label><strong>{currentUser.rollNo || 'N/A'}</strong></div>
      </div>
    </section>
  );
}
