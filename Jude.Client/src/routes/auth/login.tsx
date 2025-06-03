import Logo from "@/components/Logo";
import { login } from "@/lib/services/auth.service";
import { Alert, Button, Input } from "@heroui/react";
import authState from "@lib/state/auth.state";
import type { User } from "@lib/types/user";
import type { ApiResponse } from "@lib/utils/api";
import { publicOnlyLoader } from "@lib/utils/loaders";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import {
  createFileRoute,
  useRouter,
  useSearch
} from "@tanstack/react-router";
import { AnimatePresence, LazyMotion, domAnimation, m } from "framer-motion";
import { Eye, EyeClosed } from "lucide-react";
import { useState } from "react";

interface AuthSearchParams {
  redirect?: string;
}

export const Route = createFileRoute("/auth/login")({
  component: SignIn,
  loader: publicOnlyLoader,
  validateSearch: (search: Record<string, unknown>): AuthSearchParams => ({
    redirect: typeof search.redirect === "string" ? search.redirect : undefined,
  }),
});

function SignIn() {
  const router = useRouter();
  const search = useSearch({ from: "/auth/login" }) as AuthSearchParams;
  const redirectUrl = search.redirect || "/dashboard";

  const [errors, setErrors] = useState<string[]>([]);
  const [isVisible, setIsVisible] = useState(false);

  const variants = {
    visible: { opacity: 1, y: 0 },
    hidden: { opacity: 0, y: 10 },
  };

  const signInMutation = useMutation({
    mutationFn: (data: { userIdentifier: string; password: string }) =>
      login(data),
    onSuccess: (response: ApiResponse<User>) => {
      if (response.success) {
        authState.setState(() => ({
          user: response.data,
          isAuthenticated: true,
          isLoading: false,
        }));
        router.navigate({ to: redirectUrl, replace: true });
        return;
      } 
      setErrors(response.errors);
    },
    onError:(error: Error)=>{
      setErrors([error.message || "An unexpected error occurred."]);
    }
  });

  const form = useForm({
    defaultValues: {
      userIdentifier: "",
      password: "",
    },
    onSubmit: async ({ value }) => {
      setErrors([]);
      signInMutation.mutate(value);
    },
  });

  return (
    <div className="flex h-[100dvh] w-full items-center justify-center bg-content1">
      <div className="flex w-full max-w-xs flex-col gap-4 rounded-large mb-5 px-4 md:px-2">
        <div className="flex flex-col items-center pb-3 gap-3">
          <Logo className="h-16" />
          
          <p className="text-small text-secondary-700">
            Sign in to your account
          </p>
        </div>

        {errors.length > 0 && (
          <Alert
            color="danger"
            title="Sign-in failed"
            description={
              <ul className="list-disc ml-5 mt-1 text-xs">
                {errors.map((error, index) => (
                  <li key={index}>{error}</li>
                ))}
              </ul>
            }
            variant="bordered"
          />
        )}

        <LazyMotion features={domAnimation}>
          <AnimatePresence mode="wait">
            
              <m.form
                animate="visible"
                exit="hidden"
                initial="hidden"
                variants={variants}
                className="flex flex-col gap-4"
                onSubmit={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  form.handleSubmit();
                }}
              >
                <form.Field name="userIdentifier">
                  {(field) => (
                    <Input
                      isRequired
                      label="Username or Email"
                      type="text"
                      variant="flat"
                      size="sm"
                      value={field.state.value}
                      onChange={(e) => field.handleChange(e.target.value)}
                      isDisabled={signInMutation.isPending}
                    />
                  )}
                </form.Field>

                <form.Field name="password">
                  {(field) => (
                    <Input
                      isRequired
                      endContent={
                        <button
                          type="button"
                          onClick={() => setIsVisible(!isVisible)}
                          disabled={signInMutation.isPending}
                        >
                          {isVisible ? <EyeClosed /> : <Eye />}
                        </button>
                      }
                      label="Password"
                      type={isVisible ? "text" : "password"}
                      variant="flat"
                      size="sm"
                      value={field.state.value}
                      onChange={(e) => field.handleChange(e.target.value)}
                      isDisabled={signInMutation.isPending}
                    />
                  )}
                </form.Field>

                <Button
                  className="w-full"
                  color="primary"
                  type="submit"
                  isDisabled={signInMutation.isPending}
                  isLoading={signInMutation.isPending}
                >
                  Sign In
                </Button>
              </m.form>
            
          </AnimatePresence>
        </LazyMotion>
      </div>
    </div>
  )
}
