import { env } from "$env/dynamic/public";

export namespace ApiHelper {
	export const ApiDomain: string = (() => {
		if (!env.PUBLIC_API_DOMAIN) {
			throw new Error("PUBLIC_API_DOMAIN environment variable not defined");
		}
		return env.PUBLIC_API_DOMAIN;
	})();

	export type ProblemDetails = {
		status: number;
		type: string;
        title: string;
		detail: string;
        activityTraceId: string;
        requestTraceId: string;
	}

	export type ValidationProblemDetails = ProblemDetails & {
        errors: Record<string, string[]>
    }
}
