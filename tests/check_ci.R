library(nemsqar)
x <- 1323
n <- 1323

res_default <- nemsqar::nemsqa_binomial_confint(x, n, method = "wilson")
cat("Default: ", format(res_default$lower_ci, digits=15), "\n")

res_false <- nemsqar::nemsqa_binomial_confint(x, n, method = "wilson", correct = FALSE)
cat("Correct FALSE: ", format(res_false$lower_ci, digits=15), "\n")

res_true <- nemsqar::nemsqa_binomial_confint(x, n, method = "wilson", correct = TRUE)
cat("Correct TRUE: ", format(res_true$lower_ci, digits=15), "\n")
