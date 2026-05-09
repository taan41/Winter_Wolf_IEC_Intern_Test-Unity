public class CellMahjong : Cell
{
	public bool AllowClicking { get; private set; }

	public bool IsBottom { get; private set; }

	public void Setup(int cellX, int cellY, bool allowClicking, bool isBottom)
	{
		base.Setup(cellX, cellY);
		
		this.AllowClicking = allowClicking;
		this.IsBottom = isBottom;
	}
}