import { Button } from "@heroui/react";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
	component: App,
});

function App() {
	return (
		<div className="text-center font-bold">
			<p className="font-bold text-3xl">test</p>
			<Button variant="solid" color="primary">
				Hello
			</Button>
		</div>
	);
}
