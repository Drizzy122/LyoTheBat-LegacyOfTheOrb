using UnityEngine.UIElements;

namespace Platformer
{
    /// <summary>
    /// Wraps a HUD element in show -> hold -> close popup behavior, used by the
    /// quest banner and the level-up toast. Show() pops the element in, keeps it
    /// on screen for holdSeconds, then plays the exit transition and hides it.
    /// The animation itself lives in HUD.uss (.hud-popup / --hidden / --closing).
    /// </summary>
    public class HUDPopup
    {
        const string BaseClass = "hud-popup";
        const string HiddenClass = "hud-popup--hidden";
        const string ClosingClass = "hud-popup--closing";

        // Must cover the --closing transition duration in HUD.uss.
        const long CloseAnimMs = 400;
        // The hidden pose needs to render once before revealing, otherwise
        // UI Toolkit applies the new style instantly and skips the transition.
        const long RevealDelayMs = 50;

        enum State { Hidden, Visible, Closing }

        readonly VisualElement element;
        readonly float holdSeconds;

        readonly IVisualElementScheduledItem revealItem;
        readonly IVisualElementScheduledItem holdItem;
        readonly IVisualElementScheduledItem cleanupItem;

        State state = State.Hidden;

        /// <summary>True while the element is on screen (showing or closing).</summary>
        public bool IsVisible => state != State.Hidden;

        public HUDPopup(VisualElement element, float holdSeconds)
        {
            this.element = element;
            this.holdSeconds = holdSeconds;

            // Inline opacity (e.g. written by UI Builder) would override the
            // USS poses and kill the fade — hand control back to the classes.
            element.style.opacity = StyleKeyword.Null;

            // Start fully hidden in the pre-pop pose.
            element.AddToClassList(BaseClass);
            element.AddToClassList(HiddenClass);
            element.style.display = DisplayStyle.None;

            revealItem = element.schedule.Execute(Reveal);
            revealItem.Pause();
            holdItem = element.schedule.Execute(Close);
            holdItem.Pause();
            cleanupItem = element.schedule.Execute(FinishClose);
            cleanupItem.Pause();
        }

        /// <summary>
        /// Pops the element in and (re)starts the auto-close timer. Safe to call
        /// while already visible (extends the hold) or mid-close (glides back).
        /// </summary>
        public void Show()
        {
            switch (state)
            {
                case State.Hidden:
                    cleanupItem.Pause();
                    element.RemoveFromClassList(ClosingClass);
                    element.style.display = DisplayStyle.Flex;
                    // Let the hidden pose render, then reveal so the pop-in plays.
                    revealItem.ExecuteLater(RevealDelayMs);
                    break;

                case State.Closing:
                    // Reopened mid-close: glide back to fully visible.
                    cleanupItem.Pause();
                    element.RemoveFromClassList(ClosingClass);
                    break;

                // Already visible: just extend the timer below.
            }

            state = State.Visible;
            holdItem.ExecuteLater((long)(holdSeconds * 1000f));
        }

        /// <summary>Closes the popup now instead of waiting for the timer.</summary>
        public void Close()
        {
            if (state != State.Visible) return;
            revealItem.Pause();
            holdItem.Pause();

            // Closed before the reveal ever played — switch off instantly.
            if (element.ClassListContains(HiddenClass))
            {
                state = State.Hidden;
                element.style.display = DisplayStyle.None;
                return;
            }

            state = State.Closing;
            element.AddToClassList(ClosingClass);
            cleanupItem.ExecuteLater(CloseAnimMs);
        }

        void Reveal() => element.RemoveFromClassList(HiddenClass);

        void FinishClose()
        {
            if (state != State.Closing) return;
            state = State.Hidden;
            element.RemoveFromClassList(ClosingClass);
            element.AddToClassList(HiddenClass);
            element.style.display = DisplayStyle.None;
        }
    }
}
