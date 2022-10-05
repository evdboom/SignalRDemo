using Fh.Blz.Bsl.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;

namespace Fh.Blz.Bsl.Components.Modals
{
    public partial class Modal
    {
        [Parameter]
        public string ModalId { get; set; }
        [Parameter]
        public ObjectSize ModalSize { get; set; }
        [Parameter]
        public RenderFragment ModalTitle { get; set; }
        [Parameter]
        public RenderFragment ModalBody { get; set; }
        [Parameter]
        public RenderFragment ModalFooter { get; set; }
        [Parameter]
        public EventCallback OnClose { get; set; }
        [Parameter]
        public EventCallback OnShow { get; set; }
        [Parameter]
        public bool Draggable { get; set; }
        [Parameter]
        public bool SupportDropdowns { get; set; }

        private bool _showDialog;
        private bool _dragging;
        private bool _backgroundDown;
        private double _startX, _startY, _offsetX, _offsetY;

        public async Task Show()
        {
            _offsetX = 0;
            _offsetY = 0;
            _showDialog = true;
            await OnShow.InvokeAsync(null);
            StateHasChanged();
        }

        public async Task Close()
        {
            if (_showDialog)
            {
                await OnClose.InvokeAsync(null);
                _showDialog = false;
                StateHasChanged();
            }
        }

        public void CloseSilent()
        {
            if (_showDialog)
            {
                _showDialog = false;
                StateHasChanged();
            }
        }

        private string GetModalSize()
        {
            return ModalSize switch
            {
                ObjectSize.ExtraSmall => "modal-sm",
                ObjectSize.Small => "modal-sm",
                ObjectSize.Large => "modal-lg",
                ObjectSize.ExtraLarge => "modal-xl",
                _ => string.Empty
            };
        }

        private string SupportsDropdowns()
        {
            return SupportDropdowns ? "support-dropdown" : null;
        }

        private string DraggableClass()
        {
            return Draggable ? "draggable" : null;
        }

        private void TitleMouseDown(MouseEventArgs args)
        {
            if (!Draggable)
            {
                return;
            }

            _startX = args.ClientX - _offsetX;
            _startY = args.ClientY - _offsetY;

            _dragging = true;
        }

        private async Task BackgroundUp(MouseEventArgs args)
        {
            _dragging = false;
            if (_backgroundDown)
            {
                await Close();
            }
            _backgroundDown = false;
        }

        private void OnDrag(MouseEventArgs args)
        {
            if (_dragging)
            {
                _offsetX = args.ClientX - _startX;
                _offsetY = args.ClientY - _startY;

                StateHasChanged();
            }
        }

        private void BackgroundDown(MouseEventArgs args)
        {
            _backgroundDown = true;
        }
    }
}
