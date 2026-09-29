import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RotationPicker } from './RotationPicker';

describe('RotationPicker', () => {
  it('asks for a face when none is selected', () => {
    render(<RotationPicker face={null} busy={false} onHover={vi.fn()} onLeave={vi.fn()} onRotate={vi.fn()} />);

    expect(screen.getByText(/Click a face on the net/)).toBeInTheDocument();
    expect(screen.queryAllByRole('button')).toHaveLength(0);
  });

  it('offers the three rotations for the selected face and previews on hover', async () => {
    const onHover = vi.fn();
    const onRotate = vi.fn();
    render(<RotationPicker face="Right" busy={false} onHover={onHover} onLeave={vi.fn()} onRotate={onRotate} />);

    expect(screen.getByRole('heading')).toHaveTextContent('Turn the Right face');
    await userEvent.hover(screen.getByRole('button', { name: '90° anti-clockwise' }));
    expect(onHover).toHaveBeenCalledWith('Right', 'AntiClockwise');

    await userEvent.click(screen.getByRole('button', { name: '180°' }));
    expect(onRotate).toHaveBeenCalledWith('Right', 'Half');
  });

  it('ignores clicks while busy without taking focus away', async () => {
    const onRotate = vi.fn();
    render(<RotationPicker face="Up" busy onHover={vi.fn()} onLeave={vi.fn()} onRotate={onRotate} />);

    const option = screen.getByRole('button', { name: '180°' });
    await userEvent.click(option);

    expect(onRotate).not.toHaveBeenCalled();
    expect(option).toHaveAttribute('aria-disabled', 'true');
    expect(option).toHaveFocus();
  });
});
