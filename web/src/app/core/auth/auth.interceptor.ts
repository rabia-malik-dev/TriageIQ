import { HttpInterceptorFn } from '@angular/common/http';
import { ACCESS_TOKEN_STORAGE_KEY } from './auth-token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem(ACCESS_TOKEN_STORAGE_KEY);
  if (!token || req.url.includes('/api/auth/login')) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    }),
  );
};