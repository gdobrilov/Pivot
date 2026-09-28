import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RotationPicker } from './RotationPicker';

describe('RotationPicker', () => {
  it('asks for a face when none is selected', () => {
    render(<RotationPicker face={null} disabled={false} onHover={vi.fn()} onLeave={vi.fn()} onRotate={vi.fn()} />);

    expect(screen.getByText(/Click a face on the net/)).toBeInTheDocument();
    expect(screen.queryAllByRole('button')).toHaveLength(0);
  });

  it('offers the three rotations for the selected face and previews on hover', async () => {
    const onHover = vi.fn();
    const onRotate = vi.fn();
    render(<RotationPicker face="Right" disabled={false} onHover={onHover} onLeave={vi.fn()} onRotate={onRotate} />);

    expect(screen.getByRole('heading')).toHaveTextContent('Turn the Right face');
    await userEvent.hover(screen.getByRole('button', { name: '90° anti-clockwise' }));
    expect(onHover).toHaveBeenCalledWith('Right', 'AntiClockwise');

    await userEvent.click(screen.getByRole('button', { name: '180°' }));
    expect(onRotate).toHaveBeenCalledWith('Right', 'Half');
  });

  it('disables the options while a request is in flight', () => {
    render(<RotationPicker face="Up" disabled onHover={vi.fn()} onLeave={vi.fn()} onRotate={vi.fn()} />);

    for (const button of screen.getAllByRole('button')) {
      expect(button).toBeDisabled();
    }
  });
});
