import React from "react";
import { cn } from "@heroui/react";
import { Link, useRouterState } from "@tanstack/react-router";
import { Tooltip } from "@heroui/react";
import { DynamicIcon, type IconName } from "lucide-react/dynamic";

export type SidebarItem = {
	key: string;
	title: string;
	iconName: IconName;
	href?: string;
};

export const sidebarItems: SidebarItem[] = [
	{
		key: "dashboard",
		href: "/dashboard",
		iconName: "layout-dashboard",
		title: "Dashboard",
	},
	{
		key: "claims",
		href: "/claims",
		iconName: "file-text",
		title: "Claims",
	},
	{
		key: "knowledge",
		href: "/knowledge",
		iconName: "book-text",
		title: "Knowledge Base",
	},

];

export type SidebarProps = {
	isCompact?: boolean;
	className?: string;
};

const SidebarNav = React.forwardRef<HTMLElement, SidebarProps>(
	({ isCompact, className }, ref) => {
		const activeRoute = useRouterState({
			select: (s) => s.location.pathname.split("/")[1],
		});

		return (
			<nav ref={ref} className={cn("flex flex-col gap-2", className)}>
				{sidebarItems.map((item) => {
					const isActive = activeRoute === item.key;

					const itemContent = (
						<Link
							to={item.href || ""}
							className={cn(
								"flex items-center gap-2 px-3 min-h-11 rounded-lg transition-colors group cursor-pointer  border-transparent",
								{
									"w-12 h-11 justify-center": isCompact,
									"bg-zinc-200/60  ": isActive,
									"hover:bg-zinc-200/40": !isActive,
								},
							)}
						>
							<DynamicIcon
								size={22}
								name={item.iconName}
								className={cn({
									"text-primary": isActive,
									"text-gray-600 ": !isActive,
								})}
							/>
							{!isCompact && (	
								<span
									className={cn("text-sm font-medium", {
										"text-primary font-semibold": isActive,
										"text-gray-700 ": !isActive,
									})}
								>
									{item.title}
								</span>
							)}
						</Link>
					);

					return (
						<div key={item.key}>
							{isCompact ? (
								<Tooltip content={item.title} placement="right">
									{itemContent}
								</Tooltip>
							) : (
								itemContent
							)}
						</div>
					);
				})}
			</nav>
		);
	},
);

export default SidebarNav;
