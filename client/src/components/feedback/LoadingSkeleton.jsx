export default function LoadingSkeleton({ variant = 'rows' }) {
  const blocks = variant === 'stats' ? ['stat', 'stat', 'stat', 'stat'] : ['wide', 'wide', 'wide'];

  return (
    <div className={`loading-skeleton loading-skeleton-${variant}`} aria-label="Loading content" role="status">
      {blocks.map((block, index) => (
        <span className={`skeleton-block skeleton-${block}`} key={`${block}-${index}`} />
      ))}
    </div>
  );
}
