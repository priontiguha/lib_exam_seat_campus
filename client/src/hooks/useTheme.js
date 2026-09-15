import { useEffect, useState } from 'react';

export function useTheme(storageKey = 'libraryTheme') {
  const [theme, setTheme] = useState(() => localStorage.getItem(storageKey) || 'dark');

  useEffect(() => {
    document.body.dataset.theme = theme;
    localStorage.setItem(storageKey, theme);
  }, [storageKey, theme]);

  const toggleTheme = () => setTheme((current) => current === 'dark' ? 'light' : 'dark');

  return { theme, setTheme, toggleTheme };
}
