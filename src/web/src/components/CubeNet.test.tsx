import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CubeNet } from './CubeNet';
import { CHALLENGE_RESULT, FRONT_PREVIEW, SOLVED } from '../test/fixtures';

describe('CubeNet', () => {
  it('renders six faces with nine stickers each', () => {
    render(<CubeNet faces={SOLVED.faces} selectedFace={null} disabled={false} onSelectFace={vi.fn()} />);

    for (const face of ['Up', 'Left', 'Front', 'Right', 'Back', 'Down']) {
      expect(within(screen.getByTestId(`face-${face}`)).getAllByRole('img')).toHaveLength(9);
    }
  });

  it('shows the sticker colours of the challenge result in row-major order', () => {
    render(<CubeNet faces={CHALLENGE_RESULT.faces} selectedFace={null} disabled={false} onSelectFace={vi.fn()} />);

    const upStickers = within(screen.getByTestId('face-Up')).getAllByRole('img');
    expect(upStickers.map((sticker) => sticker.getAttribute('aria-label'))).toEqual([
      'Red', 'Orange', 'Green',
      'Blue', 'White', 'White',
      'Blue', 'Blue', 'Blue',
    ]);
  });

  it('selects a face when it is clicked', async () => {
    const onSelectFace = vi.fn();
    render(<CubeNet faces={SOLVED.faces} selectedFace="Front" disabled={false} onSelectFace={onSelectFace} />);

    await userEvent.click(screen.getByRole('button', { name: 'Right face' }));

    expect(onSelectFace).toHaveBeenCalledWith('Right');
    expect(screen.getByRole('button', { name: 'Front face' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('marks the stickers that a previewed move would change', () => {
    render(
      <CubeNet faces={SOLVED.faces} previewFaces={FRONT_PREVIEW.after} selectedFace="Front" disabled={false} onSelectFace={vi.fn()} />,
    );

    const changing = screen.getAllByRole('img', { name: /becomes/ });
    expect(changing).toHaveLength(12);
    expect(within(screen.getByTestId('face-Right')).getAllByRole('img', { name: 'Red becomes White' })).toHaveLength(3);
    expect(within(screen.getByTestId('face-Front')).queryByRole('img', { name: /becomes/ })).toBeNull();
  });
});
