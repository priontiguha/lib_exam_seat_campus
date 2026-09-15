import { useCallback, useEffect, useState } from 'react';

export function useToast(status) {
  const [toasts, setToasts] = useState([]);

  useEffect(() => {
    if (!status?.message) return undefined;

    const toast = {
      id: `${Date.now()}-${Math.random()}`,
      type: status.type || 'info',
      message: status.message,
    };
    setToasts((current) => [...current, toast].slice(-4));

    const timeout = window.setTimeout(() => {
      setToasts((current) => current.filter((item) => item.id !== toast.id));
    }, 4500);

    return () => window.clearTimeout(timeout);
  }, [status?.message, status?.type]);

  const dismissToast = useCallback((toastId) => {
    setToasts((current) => current.filter((toast) => toast.id !== toastId));
  }, []);

  return { toasts, dismissToast };
}
