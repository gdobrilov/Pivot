import { render, screen } from '@testing-library/react';
import { ActivityLog } from './ActivityLog';
import { LOG } from '../test/fixtures';

describe('ActivityLog', () => {
  it('shows an empty state', () => {
    render(<ActivityLog entries={[]} />);

    expect(screen.getByText(/No turns yet/)).toBeInTheDocument();
  });

  it('lists entries newest first with what happened and when', () => {
    render(<ActivityLog entries={LOG} />);

    const items = screen.getAllByRole('listitem');
    expect(items).toHaveLength(3);
    expect(items[0]).toHaveTextContent('Reset to solved');
    expect(items[1]).toHaveTextContent('Undo F');
    expect(items[2]).toHaveTextContent('Front 90° clockwise (F)');
    expect(items[2]?.querySelector('time')).toHaveAttribute('datetime', '2026-09-28T10:00:00Z');
  });
});
