import React from 'react';

/** Geometric mark — uses currentColor for theming */
export default function BrandMark({ size = 28, className, decorative = true }) {
  return (
    <svg
      className={className}
      width={size}
      height={size}
      viewBox="0 0 32 32"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden={decorative ? true : undefined}
      role={decorative ? 'presentation' : 'img'}
    >
      <path
        fill="currentColor"
        fillOpacity="0.92"
        d="M6 8a3 3 0 0 1 3-3h6a3 3 0 0 1 3 3v6a3 3 0 0 1-3 3H9a3 3 0 0 1-3-3V8Z"
      />
      <path
        fill="currentColor"
        fillOpacity="0.45"
        d="M17 8a3 3 0 0 1 3-3h6a3 3 0 0 1 3 3v6a3 3 0 0 1-3 3h-6a3 3 0 0 1-3-3V8Z"
      />
      <path
        fill="currentColor"
        fillOpacity="0.55"
        d="M6 19a3 3 0 0 1 3-3h6a3 3 0 0 1 3 3v6a3 3 0 0 1-3 3H9a3 3 0 0 1-3-3v-6Z"
      />
      <path
        fill="currentColor"
        fillOpacity="0.28"
        d="M17 19a3 3 0 0 1 3-3h6a3 3 0 0 1 3 3v6a3 3 0 0 1-3 3h-6a3 3 0 0 1-3-3v-6Z"
      />
    </svg>
  );
}
