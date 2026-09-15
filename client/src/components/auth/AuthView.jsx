export default function AuthView({
  loginForm,
  onLoginChange,
  onLogin,
  status,
  isSubmitting,
}) {
  return (
    <div className="auth-shell">
      <div className="auth-card">
        <div className="auth-header"><p className="eyebrow">University system</p><h1>Library &amp; Exam Portal</h1></div>
        <form onSubmit={onLogin} className="auth-form" noValidate>
          <label>Email address<input type="email" value={loginForm.email} onChange={(event) => onLoginChange('email', event.target.value)} required /></label>
          <label>Password<input type="password" value={loginForm.password} onChange={(event) => onLoginChange('password', event.target.value)} required /></label>
          <div className="demo-credentials"><small>Default admin: admin@library.edu / Admin@123</small></div>
          {status.message && <div className={`message ${status.type}`}>{status.message}</div>}
          <button type="submit" className="btn btn-primary" disabled={isSubmitting}>{isSubmitting ? 'Signing in...' : 'Login'}</button>
        </form>
      </div>
    </div>
  );
}
