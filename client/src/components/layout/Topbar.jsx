import { ChevronLeft, ChevronRight, LogOut, Moon, Search, SunMedium, UserRound } from 'lucide-react';

const sectionLabels = {
  'library-books': 'Books',
  'library-issues': 'Issues',
  'exam-rooms': 'Rooms',
  'exam-invigilators': 'Invigilators',
  'exam-schedule': 'Schedule',
  'exam-allocations': 'Allocations',
};

export default function Topbar({ activeSection, search, onSearchChange, theme, onToggleTheme, currentUser, onProfile, onLogout, collapsed, onToggleSidebar }) {
  const title = sectionLabels[activeSection] || activeSection.charAt(0).toUpperCase() + activeSection.slice(1);

  return (
    <header className="topbar">
      <div className="topbar-left">
        <button type="button" className="mobile-sidebar-toggle icon-button" onClick={onToggleSidebar} aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}>
          {collapsed ? <ChevronRight size={18} /> : <ChevronLeft size={18} />}
        </button>
        <div className="topbar-title-block"><p className="eyebrow">Campus portal</p><h2>{title}</h2></div>
      </div>
      <div className="topbar-actions">
        <label className="top-search"><Search size={16} /><input type="search" placeholder="Search..." value={search} onChange={(event) => onSearchChange(event.target.value)} /></label>
        <button type="button" className="icon-button" aria-label="Toggle theme" onClick={onToggleTheme}>{theme === 'dark' ? <SunMedium size={18} /> : <Moon size={18} />}</button>
        <button type="button" className="profile-button" onClick={onProfile}><span className="profile-avatar"><UserRound size={16} /></span><span className="profile-meta"><strong>{currentUser.name}</strong><small>{currentUser.role}</small></span></button>
        <button type="button" className="icon-button" onClick={onLogout} title="Logout"><LogOut size={18} /></button>
      </div>
    </header>
  );
}
