export default function UsersSection({ active, users, userForm, editingUserId, initialUserForm, onFormChange, onSubmit, onEdit, onDelete, onCancel, status, isSubmitting }) {
  if (!active) return null;
  return <section className="section active">
    <div className="section-header"><h1>User Management</h1><p>Create, edit, and manage system users</p></div>
    {status.message && <div className={`message ${status.type}`}>{status.message}</div>}
    <div className="form-card"><h2>{editingUserId ? 'Edit user' : 'Create user'}</h2><form onSubmit={onSubmit} className="form-group">
      <label>Full name<input value={userForm.name} onChange={(event) => onFormChange('name', event.target.value)} required /></label>
      <div className="form-row"><label>Roll number<input value={userForm.rollNo} onChange={(event) => onFormChange('rollNo', event.target.value)} required /></label><label>Department<input value={userForm.dept} onChange={(event) => onFormChange('dept', event.target.value)} required /></label></div>
      <div className="form-row"><label>Semester<select value={userForm.semester} onChange={(event) => onFormChange('semester', event.target.value)}>{[1,2,3,4,5,6,7,8].map((item) => <option key={item} value={item}>{item}</option>)}</select></label><label>Role<select value={userForm.role} onChange={(event) => onFormChange('role', event.target.value)}><option>Student</option><option>Librarian</option><option>Exam Coordinator</option><option>Admin</option></select></label></div>
      <label>Email address<input type="email" value={userForm.email} onChange={(event) => onFormChange('email', event.target.value)} required /></label>
      <label>Password<input type="password" value={userForm.password} onChange={(event) => onFormChange('password', event.target.value)} placeholder={editingUserId ? 'Leave blank to keep current password' : 'Enter password'} /></label>
      <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}><input type="checkbox" checked={userForm.isVerified} onChange={(event) => onFormChange('isVerified', event.target.checked)} />Verified user</label>
      <div className="form-actions"><button type="submit" className="btn btn-primary" disabled={isSubmitting}>{isSubmitting ? (editingUserId ? 'Saving...' : 'Creating...') : (editingUserId ? 'Save changes' : 'Create user')}</button>{editingUserId && <button type="button" className="btn btn-secondary" onClick={onCancel}>Cancel</button>}</div>
    </form></div>
    <div className="table-container"><div className="table-header"><h2>All users</h2></div><table><thead><tr><th>Name</th><th>Role</th><th>Email</th><th>Department</th><th>Actions</th></tr></thead><tbody>{users.map((user) => <tr key={user.studentId}><td>{user.name}</td><td>{user.role}</td><td>{user.email}</td><td>{user.dept}</td><td><div className="action-group"><button type="button" className="btn btn-secondary" onClick={() => onEdit(user)}>Edit</button><button type="button" className="btn btn-destructive" onClick={() => onDelete(user.studentId)}>Delete</button></div></td></tr>)}</tbody></table></div>
  </section>;
}
