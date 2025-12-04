import { Button as AriaButton, type ButtonProps } from 'react-aria-components';
import { tv } from 'tailwind-variants';
import { twMerge } from 'tailwind-merge';

// Используем tv для создания вариантов стилей
const buttonStyles = tv({
  base: [
    'px-5 py-2 text-base rounded-lg font-semibold outline-none transition-all duration-150',
    // Стили для состояния фокуса (важно для доступности)
    'focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-blue-500 dark:focus-visible:ring-offset-gray-800',
    // Стили для disabled состояния
    'disabled:bg-gray-300 dark:disabled:bg-gray-600 disabled:text-gray-500 dark:disabled:text-gray-400 disabled:cursor-not-allowed',
  ],
  variants: {
    variant: {
      primary: [
        'bg-blue-600 text-white shadow-md',
        'hover:bg-blue-700',
        'pressed:bg-blue-800 pressed:scale-[0.98]', // React Aria предоставляет data-pressed="true"
      ],
      secondary: [
        'bg-gray-200 text-gray-800 border border-gray-300',
        'hover:bg-gray-300',
        'pressed:bg-gray-400 pressed:scale-[0.98]',
        'dark:bg-gray-700 dark:text-gray-200 dark:border-gray-600 dark:hover:bg-gray-600',
      ],
      destructive: [
        'bg-red-600 text-white shadow-md',
        'hover:bg-red-700',
        'pressed:bg-red-800 pressed:scale-[0.98]',
      ],
    },
  },
  defaultVariants: {
    variant: 'primary',
  },
});

// Расширяем стандартные пропсы кнопки, чтобы принимать наши варианты
interface CustomButtonProps extends ButtonProps {
  variant?: 'primary' | 'secondary' | 'destructive';
}

// Наш компонент-обертка
export function Button({ variant, className, ...props }: CustomButtonProps) {
  return (
    <AriaButton
      {...props}
      className={(values) =>
        twMerge(
          buttonStyles({ variant }),
          typeof className === 'function' ? className(values) : className
        )
      }
    />
  );
}